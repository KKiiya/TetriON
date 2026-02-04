using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using TetriON.Client.Abstraction.Media;

namespace TetriON.Client.Abstraction;

public interface ISkinManager : IDisposable {

    bool HasCustomTexture(string path);
    Texture2D LoadCustomTexture(string path);
    bool HasCustomSound(string name);
    SoundEffect LoadCustomSoundEffect(string name);
    (bool success, ITexture texture) GetTextureAsset(string texturename, bool debug = false);
    IFont GetFontAsset(string fontname, bool debug = false);
    ISound GetAudioAsset(string soundname, bool debug = false);
    ISong? GetSongAsset(string songname, bool debug = false);
    string[] GetValidTextureNames();
    string[] GetValidSoundNames();
    string[] GetValidSongNames();
    string GetSkinPath();
    void LoadAllAssets();

    // Song variant management methods
    string[] GetSongVariants(string baseName);
    string GetRandomSongVariant(string baseName);
    bool HasSongVariants(string baseName);
    string[] GetAvailableSongs();
    bool HasCustomSong(string songName);
}

