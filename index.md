# Sound Import
A guide on how to import custom sounds into Battlefront II.

Example mod: [Kylo's Gamer Moment](https://mega.nz/#!upwBCKRa!h0HnJXOe_3WipsVoJr16cx3oMmLPCG9b-BFEQ40SOc8)

Example mod project file: [Kylo's Gamer Moment](https://mega.nz/#!G44w0CTI!YKTPI8yBT38G1fHVu7oi9Mgb5dUjZp2NPegJLckKV3E)

## New Button Injector!
Sound Import now comes with a utility program that hosts FrostyEd.exe and injects an import button into the sound editor.

Make sure `dandev-el3.exe` (version 1.1), `FrostyEd.exe` and `FrostySoundImport.exe` are all in the same directory.

Close and re-open the sound tab after importing to see the changes in the editor. 

<header style="position: relative; float: none;">
  <ul>
    <li style="width: 89px; border-right: 0px;"><a href="https://github.com/DanielElam/bf2-sound-import/releases/download/1.3.1/FrostySoundImport-1.3.1.2.zip">Download <strong>ZIP File</strong></a></li>
  </ul>
</header>

# Manual Sound Import Guide

## Prerequisites

To import custom sounds into Battlefront II you'll need 2 other programs:
- Audacity
- Frosty Mod Editor

You'll also need to have the latest version of the VC++ runtime installed: (download vc_redist.x64.exe)
[https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads](https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads)

## Installation
<header style="position: relative; float: none;">
  <ul>
    <li style="width: 89px; border-right: 0px;"><a href="https://github.com/DanielElam/bf2-sound-import/releases/download/1.1/dandev-el3.zip">Download <strong>ZIP File</strong></a></li>
  </ul>
</header>

Extract the zipped files into an empty folder, you should save your sound files in the same folder for simplicity.

# Save MP3
* Open your sound file in Audacity.
* Change the Project Rate at the bottom left of Audacity to **48000**.
* Click File > Export > Export as MP3
* Set Bit Rate Mode to **Constant** and Quality to **320kbps**. You can experiment with different settings but these work for me.
If you are replacing a mono sound (like an emote or voice line), tick **Force export to mono**
* Save the file.

# Encode as chunk
* Open command line and CD to your folder.
* `dandev-el3 YOUR-FILE-NAME.mp3 -o YOUR-FILE-NAME.chunk`
* You'll need to enter the ChunkSize so take a note of it.

# Import chunk
* Navigate to the sound you want to replace. (e.g. Sound/VO/MP/Hero/Rey/Core/SW02_VO_MP_Hero_Rey_Core_Emote4)
* Open "Chunks" and expand the chunk for your localisation. 
* Change the ChunkSize to the ChunkSize shown in the command line.
* Copy ChunkID
* Open the relevant segment and change SegmentLength to the total length of your sound in seconds.
* Go to Tools > RES/Chunk Explorer
* Paste the ChunkID into the search bar
* Right click the chunk > Import
* Select your .chunk file

Your sound should now play in-game.

# Support
If you'd like to support me and say thanks you can [buy me a ~~red bull~~ coffee](http://buymeacoff.ee/dandev)
