#!/bin/bash

VERSION=""

# get parameters
while getopts v: flag
do
  case "${flag}" in
    v) VERSION=${OPTARG};;
  esac
done

# get highest tag number, and add v0.1.0 if doesn't exist
git fetch --prune --unshallow 2>/dev/null
CURRENT_VERSION=`git describe --abbrev=0 --tags 2>/dev/null`

if [[ $CURRENT_VERSION == '' ]]
then
  CURRENT_VERSION='v0.1.0'
fi
echo "Current Version: $CURRENT_VERSION"

# Убираем 'v' из начала для числовых операций
CURRENT_VERSION_NO_V=${CURRENT_VERSION#v}
echo "Version without v: $CURRENT_VERSION_NO_V"

# replace . with space so can split into an array
CURRENT_VERSION_PARTS=(${CURRENT_VERSION_NO_V//./ })

# get number parts
VNUM1=${CURRENT_VERSION_PARTS[0]}
VNUM2=${CURRENT_VERSION_PARTS[1]}
VNUM3=${CURRENT_VERSION_PARTS[2]}

# Сохраняем оригинальные значения для проверки
ORIG_VNUM1=$VNUM1
ORIG_VNUM2=$VNUM2
ORIG_VNUM3=$VNUM3

if [[ $VERSION == 'major' ]]
then
  VNUM1=$((VNUM1 + 1))
  VNUM2=0
  VNUM3=0
elif [[ $VERSION == 'minor' ]]
then
  VNUM2=$((VNUM2 + 1))
  VNUM3=0
elif [[ $VERSION == 'patch' ]]
then
  VNUM3=$((VNUM3 + 1))
else
  echo "No version type (https://semver.org/) or incorrect type specified, try: -v [major, minor, patch]"
  exit 1
fi

# create new tag
NEW_TAG="v$VNUM1.$VNUM2.$VNUM3"

# ВАЖНО: Проверка что версия увеличилась правильно
if [[ $VERSION == 'minor' && $VNUM2 -eq $ORIG_VNUM2 ]]; then
  echo "❌ Error: Minor version didn't increase!"
  exit 1
fi

if [[ $VERSION == 'patch' && $VNUM3 -eq $ORIG_VNUM3 ]]; then
  echo "❌ Error: Patch version didn't increase!"
  exit 1
fi

if [[ $VERSION == 'major' && $VNUM1 -eq $ORIG_VNUM1 ]]; then
  echo "❌ Error: Major version didn't increase!"
  exit 1
fi

echo "($VERSION) updating $CURRENT_VERSION to $NEW_TAG"

case $VERSION in
  major)
    EXPECTED="v$((ORIG_VNUM1+1)).0.0"
    ;;
  minor)
    EXPECTED="v$ORIG_VNUM1.$((ORIG_VNUM2+1)).0"
    ;;
  patch)
    EXPECTED="v$ORIG_VNUM1.$ORIG_VNUM2.$((ORIG_VNUM3+1))"
    ;;
esac

if [[ "$NEW_TAG" != "$EXPECTED" ]]; then
  echo "❌ Version calculation error!"
  echo "Expected: $EXPECTED"
  echo "Got: $NEW_TAG"
  exit 1
fi

# get current hash and see if it already has a tag
GIT_COMMIT=`git rev-parse HEAD`
NEEDS_TAG=`git describe --contains $GIT_COMMIT 2>/dev/null`

# only tag if no tag already
if [ -z "$NEEDS_TAG" ]; then
  echo "Tagged with $NEW_TAG"
  git tag $NEW_TAG
  git push --tags
  git push
else
  echo "Already a tag on this commit"
fi

if [ -n "$GITHUB_OUTPUT" ]; then
  echo "new-tag=$NEW_TAG" >> $GITHUB_OUTPUT
else
  echo "new-tag=$NEW_TAG"
fi

exit 0
