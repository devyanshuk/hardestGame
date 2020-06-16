csc -lib:/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/gtk-sharp-2.0 \
    -r:gdk-sharp.dll,glib-sharp.dll,gtk-sharp.dll,Mono.Cairo.dll \
    codes/player.cs codes/obstacle.cs codes/view.cs codes/movement.cs \
    codes/game.cs codes/checkpoints.cs codes/parser.cs -out:game.exe;
mono game.exe
