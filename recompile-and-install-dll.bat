@"C:\Program Files\Unity\Hub\Editor\2017.4.30f1\Editor\Data\MonoBleedingEdge\lib\mono\4.5\mcs.exe" -sdk:2 -target:library -out:Assembly-CSharp.dll "@dep_mcs.txt" "@files.txt"
@rizin -w -c "wx 35 @ 0x400000" -qq Assembly-CSharp.dll
@adb push Assembly-CSharp.dll /data/local/tmp
@adb shell su -c cp /data/local/tmp/Assembly-CSharp.dll /data/data/com.solarmax3.guanwangtlm/files/game_ass.dll
@rem @rm Assembly-CSharp.dll
@echo "Completed!!!"
