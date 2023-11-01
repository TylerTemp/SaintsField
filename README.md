# ExtInspector #

## Development ##

1.  `git clone --branch unity ${your-folk-git-url} SaintsField`
2.  `git rm Assets/SaintsField`
3.  `rm -rf .git/modules/SaintsField`
4.  `git config --remove-section submodule.Assets/SaintsField`
5.  `git submodule add --force ${your-folk} Assets/SaintsField`
6.  `cd Assets/SaintsField` and checkout to your editing branch, e.g. `git checkout master`
7.  windows: `.\link.cmd`

    mac/linux: `ln -s 'Assets/ExtInspector/Samples~' 'Assets/ExtInspector/Samples'`

## Release ##

```bash
cd Assets/ExtInspecor
git add .
// ... as per usual
```
