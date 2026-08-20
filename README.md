# PS2RD CHT Manager

PS2RD CHT Manager is a Windows application for creating, editing and managing `.cht` cheat files for **Open PS2 Loader (OPL) / PS2RD**.

## Features

- Open PS2 ISO and ELF files
- Detect Game ID and mastercode
- Open and edit existing `.cht` files
- Add, edit, delete and reorder cheats
- Support for cheat descriptions
- Import cheats from `.pnach` files
- Select individual cheats during PNACH import
- Search, Select All and Deselect All during import
- Duplicate RAW-code detection
- PS2RD RAW-code validation
- Detection/filtering of unsupported or potentially encrypted codes
- Save PS2RD-compatible `.cht` files
- Move cheats Up/Down while keeping Game ID and Mastercode fixed at the top

## Requirements

- Windows 10/11
- .NET 8

## Credits / Sources

PS2RD CHT Manager uses or is based on information and conversion logic from the following open-source projects:

- **PS2RD**  
  PS2 remote debugger and cheat engine.  
  https://github.com/mlafeldt/ps2rd

- **PS2 PNACH Converter**  
  PNACH parsing/conversion logic used as a reference for PNACH importing.  
  https://github.com/israpps/PS2-pnach-converter

- **CB2crypt**  
  Reference information for PlayStation 2 CodeBreaker code encryption/decryption.  
  https://github.com/mlafeldt/CB2crypt

- **OmniConvert**  
  Reference for PS2 cheat-code formats and conversion between different cheat devices.  
  https://github.com/pyriell/omniconvert

## Disclaimer

PS2RD CHT Manager is an independent community project and is not affiliated with Sony, the PCSX2 project, Open PS2 Loader, or the authors of the referenced projects.

Always verify imported cheat codes before using them. Unsupported or encrypted cheat formats may require conversion before they can be used with PS2RD.
