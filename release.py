"""
Usage:
    release.py dist
    release.py back

dist: copy needed files to dist with required struct
back: run release.py UNDER the dist folder, to put the struct back to upm requried struct
"""

import os
import sys
import shutil
import logging


root = os.path.normpath(os.path.abspath(os.path.dirname(__file__)))
# print(root)


def copytreer(src, dst, symlinks=False, ignore=None):
    os.makedirs(dst, exist_ok=True)
    for item in os.listdir(src):
        s = os.path.join(src, item)
        d = os.path.join(dst, item)
        if os.path.isdir(s):
            shutil.copytree(s, d, symlinks, ignore, dirs_exist_ok=True)
        else:
            shutil.copy2(s, d)

def dist():
    dist = os.path.join(root, "dist")
    shutil.rmtree(dist)
    os.makedirs(dist)
    with open(os.path.join(dist, '.placeholder'), 'wb') as _:
        pass
    
    logging.debug(f'Samples~')
    copytreer(os.path.join(root, 'Assets', 'Scripts', 'ExtInspector', 'Samples'), os.path.join(dist, 'Samples~'))
    shutil.copy(os.path.join(root, 'Assets', 'Scripts', 'ExtInspector', 'Samples.meta'), os.path.join(dist, 'Samples~.meta'))

    logging.debug(f'Scripts')
    
    copytreer(os.path.join(root, 'Assets', 'Editor Default Resources'), os.path.join(dist, 'Scripts', 'Editor Default Resources'))
    shutil.copyfile(os.path.join(root, 'Assets', 'Editor Default Resources.meta'), os.path.join(dist, 'Scripts', 'Editor Default Resources.meta'))

    copytreer(os.path.join(root, 'Assets', 'Scripts', 'ExtInspector'), os.path.join(dist, 'Scripts'))
    shutil.copyfile(os.path.join(root, 'Assets', 'Scripts', 'ExtInspector.meta'), os.path.join(dist, 'Scripts.meta'))

    shutil.rmtree(os.path.join(dist, 'Scripts', 'Samples'))
    os.remove(os.path.join(dist, 'Scripts', 'Samples.meta'))

    os.remove(os.path.join(dist, 'Scripts', 'package.json'))
    os.remove(os.path.join(dist, 'Scripts', 'package.json.meta'))

    logging.debug(f'package.json/readme')
    shutil.copyfile(os.path.join(root, 'Assets', 'Scripts', 'ExtInspector', 'package.json'), os.path.join(dist, 'package.json'))

    shutil.copyfile(os.path.join(root, 'Assets', 'Scripts', 'ExtInspector', 'package.json'), os.path.join(dist, 'package.json'))
    shutil.copyfile(os.path.join(root, 'README.md'), os.path.join(dist, 'README.md'))

    shutil.copyfile(__file__, os.path.join('dist', 'release.py'))
    return 0


def back():
    project = os.path.dirname(root)

    (_, folders, files) = next(os.walk(project))

    for folder in folders:
        if folder.startswith('.') or folder == 'dist':
            continue
        logging.debug(f'rm -rf {folder}')
        shutil.rmtree(os.path.join(project, folder))
    
    for file_name in files:
        if file_name.startswith('.'):
            continue
        logging.debug(f'rm {file_name}')
        os.remove(os.path.join(project, file_name))

    # print(files)
    # print(folders)

    logging.debug(f'cp -r "{root}" "{project}"')
    copytreer(root, project)

    logging.debug(f'rm release.py')
    os.remove(os.path.join(project, 'release.py'))


def error_exit():
    sys.stderr.write(__doc__)
    sys.exit(1)


if __name__ == '__main__':
    logging.basicConfig(level=logging.DEBUG)

    if(len(sys.argv) == 0):
        error_exit()

    param = sys.argv[1]
    if param == 'dist':
        sys.exit(dist())
    elif param == 'back':
        sys.exit(back())
    
    error_exit()
