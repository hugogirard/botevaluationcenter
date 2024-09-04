az appconfig kv import -s file --format json --content-type "application/json" --name appconfigai --path kbconf.json --yes

az appconfig kv import -s file --format json --label dev --content-type "application/json" --separator : --depth 2 --name appconfigai --path subs.json --yes


https://github.com/microsoft/botbuilder-samples?tab=readme-ov-file