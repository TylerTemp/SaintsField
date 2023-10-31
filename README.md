# ExtInspector #

## Development ##

1.  `git clone --branch unity ${your-folk-git-url} ExtInspectorUnity`
2.  `git rm Assets/ExtInspector`
3.  `rm -rf .git/modules/ExtInspector`
4.  `git config --remove-section submodule.Assets/ExtInspector`
5.  `git submodule add ${your-folk} Assets/ExtInspector`
6.  `cd Assets/ExtInspector` and checkout to your editing branch, e.g. `git checkout master`
7.  windows: `.\link.cmd`

    mac/linux: `ln -s 'Assets/ExtInspector/Samples~' 'Assets/ExtInspector/Samples'`

## Release ##

```bash
cd Assets/ExtInspecor
git add .
// ... as per usual
```
