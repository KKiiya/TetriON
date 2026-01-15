Custom Skin Instructions:
========================

1. Create a new folder in the 'skins' directory with your skin name
2. Add PNG files for your custom textures:
   - tiles.png (for tetromino blocks)
   - background.png (optional background)
   - ui.png (optional UI elements)

3. Add audio files for your custom sounds:
   Game Actions: move.wav, rotate.wav, harddrop.wav, hold.wav, spin.wav
   Line Clears: clearline.wav, clearquad.wav, clearspin.wav, allclear.wav
   Combos: combo_1.wav through combo_16.wav
   Menu: menuclick.wav, menutap.wav
   And many more! See ValidSoundNames in SkinManager for full list.

4. The game will automatically detect and load your custom skin
5. Use LoadCustomTexture("filename") to load your PNG files
6. Use LoadCustomSound("filename") to load your audio files

Example structure:
skins/
  default/
    tiles.png
    move.wav
  myskin/
    tiles.png
    background.png
    move.mp3
    rotate.ogg