cd ./4craftThreadWatcher/builds
while true;
do
killall mono;
mono --debug 4craftThreadWatcher.exe;
done
