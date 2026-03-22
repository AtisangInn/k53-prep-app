import os

unused_images_file = r'c:\tmp\unused_images.txt'

if not os.path.exists(unused_images_file):
    print("Unused images file not found.")
    exit(1)

deleted_count = 0
errors = []

with open(unused_images_file, 'r', encoding='utf-8') as f:
    for line in f:
        file_path = line.strip()
        if os.path.exists(file_path):
            try:
                os.remove(file_path)
                deleted_count += 1
            except Exception as e:
                errors.append(f"Error deleting {file_path}: {e}")
        else:
            print(f"File not found: {file_path}")

print(f"Successfully deleted {deleted_count} files.")
if errors:
    print("\nErrors encountered:")
    for err in errors:
        print(err)
