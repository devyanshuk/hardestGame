CODES = codes/*.cs
LEVELS = levels/*.txt

.PHONY: clean

game.exe: $(CODES) $(LEVELS)
	csc $^ `pkg-config --libs gtk-sharp-2.0` -r:Mono.Cairo.dll -out:$@

clean:
	rm -f *.exe
