Custom Skin Instructions:
========================

1. Create a new folder in the 'skins' directory with your skin name
2. Add PNG files for your custom textures:
   - tiles.png (for tetromino blocks, 29x29 with 1 pixel spacing)
   - background.png (optional background)
   - ui.png (optional UI elements)

3. Add audio files for your custom sounds:
   - move.wav/.mp3/.ogg (piece movement sound)
   - rotate.wav/.mp3/.ogg (piece rotation sound)
   - clear.wav/.mp3/.ogg (line clear sound)
   - drop.wav/.mp3/.ogg (hard drop sound)

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