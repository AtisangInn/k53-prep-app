import os

img_dir = r'c:\Users\kings\AppData\Roaming\Microsoft\Windows\Libraries\Downloads\K53PrepApp\K53PrepApp\frontend\assets\img'
project_dir = r'c:\Users\kings\AppData\Roaming\Microsoft\Windows\Libraries\Downloads\K53PrepApp\K53PrepApp'

extensions = ('.png', '.jpg', '.jpeg', '.svg', '.webp')
source_extensions = ('.html', '.js', '.css', '.cs')

# 1. Get all images in the directory
images = [f for f in os.listdir(img_dir) if f.lower().endswith(extensions)]

# 2. Collect all source content to search through
all_source_content = ""
for root, dirs, files in os.walk(project_dir):
    # Skip binary and build directories
    if 'bin' in dirs: dirs.remove('bin')
    if 'obj' in dirs: dirs.remove('obj')
    if '.git' in dirs: dirs.remove('.git')
    
    for file in files:
        if file.lower().endswith(source_extensions):
            file_path = os.path.join(root, file)
            try:
                with open(file_path, 'r', encoding='utf-8') as f:
                    all_source_content += f.read() + "\n"
            except Exception as e:
                print(f"Error reading {file_path}: {e}")

# 3. Identify unused images
unused_images = []
for img in images:
    if img not in all_source_content:
        unused_images.append(img)

# 4. Output results
print(f"Total images found: {len(images)}")
print(f"Unused images found: {len(unused_images)}")
print("\nUnused images:")
for unused in unused_images:
    print(unused)

# Write to a file for deletion script to use
with open(r'c:\tmp\unused_images.txt', 'w', encoding='utf-8') as f:
    for unused in unused_images:
        f.write(os.path.join(img_dir, unused) + "\n")
