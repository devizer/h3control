file=$(command -v nuget)
echo NUGET IS HERE: $file
cat "$file"
nuget help | head -1
