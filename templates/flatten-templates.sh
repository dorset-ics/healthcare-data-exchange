#!/bin/bash

dest_dir=$(pwd)

function replace_slash_with_underscore {
   echo "$1" | tr '/' '_'
}

for dir in */; do
    [[ $dir =~ _ ]] && continue

    find "$dir" -type f | while read -r file; do
        relative_path=${file#./}
        new_file_name=$(replace_slash_with_underscore "$relative_path")
        cp "$file" "$dest_dir/$new_file_name"
    done

    rm -rf "$dir" 
done