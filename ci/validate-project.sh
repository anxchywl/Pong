#!/usr/bin/env bash

set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$project_root"

status=0

while IFS= read -r -d '' asset; do
  if [[ ! -f "$asset.meta" ]]; then
    echo "missing meta file: $asset.meta" >&2
    status=1
  fi
done < <(find Assets -type f ! -name '*.meta' -print0)

while IFS= read -r -d '' meta; do
  asset="${meta%.meta}"
  if [[ ! -e "$asset" ]]; then
    echo "orphaned meta file: $meta" >&2
    status=1
  fi
done < <(find Assets -name '*.meta' -print0)

duplicate_guids="$(find Assets -name '*.meta' -type f -exec awk '/^guid: / { print $2 }' {} + | sort | uniq -d)"
if [[ -n "$duplicate_guids" ]]; then
  echo "duplicate Unity asset GUIDs:" >&2
  echo "$duplicate_guids" >&2
  status=1
fi

tracked_generated="$(git ls-files | grep -E '^(Library|Temp|Logs|obj|UserSettings|Build|Builds)/' || true)"
if [[ -n "$tracked_generated" ]]; then
  echo "generated Unity files are tracked:" >&2
  echo "$tracked_generated" >&2
  status=1
fi

python3 -m json.tool Packages/manifest.json >/dev/null
python3 -m json.tool Packages/packages-lock.json >/dev/null

build_scene_count="$(awk '/^[[:space:]]+path: Assets\// { count++ } END { print count + 0 }' ProjectSettings/EditorBuildSettings.asset)"
build_scene="$(awk '/^[[:space:]]+path: Assets\// { print $2; exit }' ProjectSettings/EditorBuildSettings.asset)"
if [[ "$build_scene_count" -ne 1 || ! -f "$build_scene" ]]; then
  echo "EditorBuildSettings must reference exactly one existing scene" >&2
  status=1
fi

exit "$status"
