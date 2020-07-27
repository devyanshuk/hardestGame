CODES = codes/*.cs

.PHONY: clean

game.exe: $(CODES)
	csc $^ `pkg-config --libs gtk-sharp-2.0` -r:Mono.Cairo.dll -out:$@

clean:
	rm *.exe
