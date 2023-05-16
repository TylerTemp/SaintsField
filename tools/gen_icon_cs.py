import os

project_root = os.path.normpath(os.path.join(__file__, '..', '..'))

icon_cs = os.path.join(project_root, 'Assets', 'Scripts', 'ExtInspector', 'Standalone', 'Icon.cs')
icon_folder = os.path.join(project_root, 'Assets', 'Editor Default Resources', 'ExtInspector', 'fa')
# print(icon_folder)

_, _, files = next(os.walk(icon_folder))
# print(files)

file_base_names = []
for each_file in files:
    file_base, file_ext = os.path.splitext(each_file)
    if file_ext == '.png':
        file_base_names.append(file_base)
    
# print(file_base_names)

with open(icon_cs, 'a+', encoding='utf-8') as f:
    f.seek(0)
    lines = []
    in_fa = False
    for line in f:
        if not in_fa:
            lines.append(line)
        if line.strip() == '#region Gen FA':
            in_fa = True
            for each_file in file_base_names:
                # ExtInspector/eye-regular.png
                var_name = f"{each_file.replace('-', '_').upper()}"
                lines.append(f'            public const string {var_name} = "ExtInspector/fa/{each_file}.png";\n')
        elif in_fa and line.strip() == '#endregion':
            in_fa = False
            lines.append(line)
    
    f.seek(0)
    f.truncate()
    f.writelines(lines)

