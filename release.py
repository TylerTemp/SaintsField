"""
Usage:
    release.py dist
    release.py back
"""

import os
import sys
import shutil
import logging


root = os.path.normpath(os.path.abspath(os.path.dirname(__file__)))


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

    logging.debug(f'package.json/readme')
    shutil.copyfile(os.path.join(root, 'Assets', 'Scripts', 'ExtInspector', 'package.json'), os.path.join(dist, 'package.json'))

    shutil.copyfile(os.path.join(root, 'Assets', 'Scripts', 'ExtInspector', 'package.json'), os.path.join(dist, 'package.json'))
    shutil.copyfile(os.path.join(root, 'README.md'), os.path.join(dist, 'README.md'))

    shutil.copyfile(__file__, os.path.join('dist', 'release.py'))
    return 0


if __name__ == '__main__':
    logging.basicConfig(level=logging.DEBUG)

    param = sys.argv[1]
    if param == 'dist':
        sys.exit(dist())
    
    sys.stderr.write(__doc__)
    sys.exit(1)
