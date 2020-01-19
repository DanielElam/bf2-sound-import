## Prerequisites

To import custom sounds into Battlefront II you'll need 2 other programs:
- Audacity
- Frosty Mod Editor

## Installation
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
