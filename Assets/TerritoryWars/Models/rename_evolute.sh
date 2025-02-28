#!/bin/bash

# Set error handling
set -e

echo "Starting file renaming and content modification process..."

# Function to check if any matching files exist
check_files_exist() {
    local pattern=$1
    if ! ls $pattern &> /dev/null; then
        echo "No files matching pattern '$pattern' found."
        return 1
    fi
    return 0
}

# Step 1: Rename files by replacing 'evolute_duel-E' with 'evolute_duel-'
echo "Step 1: Renaming files..."
if check_files_exist "evolute_duel-E*"; then
    for file in evolute_duel-E*; do
        if [ -f "$file" ]; then
            new_name="${file/evolute_duel-E/evolute_duel-}"
            mv "$file" "$new_name"
            echo "Renamed: $file -> $new_name"
        fi
    done
    echo "File renaming completed successfully."
fi

# Step 2: Replace content in .gen.cs files
echo -e "\nStep 2: Modifying file contents..."
if check_files_exist "*.gen.cs"; then
    # First check if any files contain the pattern
    if grep -l "evolute_duel_E" *.gen.cs &> /dev/null; then
        # Perform the replacement
        if sed -i 's/evolute_duel_E/evolute_duel_/g' *.gen.cs; then
            echo "Content replacement completed successfully."
            
            # Verify no instances remain
            if grep "evolute_duel_E" *.gen.cs &> /dev/null; then
                echo "Warning: Some instances of 'evolute_duel_E' might still exist."
            else
                echo "Verified: All instances have been replaced."
            fi
        else
            echo "Error: Failed to replace content in files."
            exit 1
        fi
    else
        echo "No files contain the pattern 'evolute_duel_E'."
    fi
fi

echo -e "\nAll operations completed successfully!"

