# Script to make targeted edits to Layout.tsx
import re

# Read the file
with open('D:\\Program Files\\.copilot\\copilot-worktrees\\POS\\thihakyaw-leo-urban-spoon\\apps\\web\\src\\saaS-console\\Layout.tsx', 'r') as f:
    content = f.read()

# Make the changes
# Change px-4 py-2 to px-6 py-4 (non-mobile variant)
content = re.sub(r'px-4 py-2', 'px-6 py-4', content)

# Change px-5 to px-6 (mobile variant)
content = re.sub(r'px-5\s+mobile:px-5', 'px-6 mobile:px-6', content)

# Change py-5 to py-6 (mobile variant)
content = re.sub(r'mobile:py-5\s+mobile:min-w-10', 'mobile:py-6 mobile:min-w-16', content)

# Write the file back
with open('D:\\Program Files\\.copilot\\copilot-worktrees\\POS\\thihakyaw-leo-urban-spoon\\apps\\web\\src\\saaS-console\\Layout.tsx', 'w') as f:
    f.write(content)

print("Edits made successfully!")