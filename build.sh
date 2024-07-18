#!/usr/bin/env bash

files=( OTD.UX.Remote
        OTD.UX.Remote.Lib
        OpenTabletDriver.External.Common )

prepare_build_folder()
{
    if [ ! -d "build/$1" ]; then
        mkdir "build/$1" --parents
    else
        rm -rf "build/$1/*"
    fi
}

move_if_exists()
{
    if [ -f "$1" ]; then
        cp $1 $2
    fi
}

# Re-create hashes.txt
> "./build/hashes.txt"

for version in 0.5.x 0.6.x
do
    # Build & Exit on failure
    dotnet build OTD.UX.Remote-$version -c Debug -o temp/$version || exit 1

    # check if the build folder does not exist, else create it
    prepare_build_folder $version

    # Move necessary files to the build folder
    for file in "${files[@]}"
    do
        move_if_exists temp/$version/$file.dll build/$version
        move_if_exists temp/$version/$file.pdb build/$version
    done

    (
        cd build/$version
        jar -cfM OTD.UX.Remote-$version.zip *.dll

        echo "Computing OTD.UX.Remote-$version.zip Hash"
        sha256sum OTD.UX.Remote-$version.zip >> ../hashes.txt
    )
done

# Remove the temp folder
rm -rf temp