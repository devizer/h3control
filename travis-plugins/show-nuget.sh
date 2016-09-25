file=$(command -v nuget)
echo NUGET IS HERE: $file
if test "$TRAVIS_OS_NAME" == "linux"; then
  cat "$file";
  nuget help;
fi

