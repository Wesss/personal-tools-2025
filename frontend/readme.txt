
# (alias for nvm may be winpty nvm already)
winpty nvm install 24.5.0
winpty nvm use 24.5.0




# If 'winpty npm' gives "winpty: error: cannot start 'npm': Not found in PATH"
# THIS IS NOT WORKING, worked around with adding alias npm="winpty \"C:\Program Files\nodejs\npm.cmd\""
- test with npm location with
winpty "C:\Users\wesle\AppData\Roaming\nvm\v24.5.0\npm.cmd" -v
- add %USERPROFILE%\AppData\Roaming\nvm\v24.5.0 to path, google how to do on windows
- restart terminal
- then install npm?
winpty npm.cmd install


# creating vite project
npm create vite@latest
- name MtgManager
cd MtgManager
npm install
npm run dev
- go to http://localhost:5173/

