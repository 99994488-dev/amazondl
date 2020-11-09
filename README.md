# amazondl
Tool for downloading drm-encrypted content from Amazon Prime upto 1080p in CBR and VBR and with audio upto 640 kb/s.

Source code will be uploaded soon

## Usage
Put your Amazon cookies in a file inside the "cookies" folder in the program's working directory. The cookie file should be a Netscape cookie format .txt and its name should be the region it is from, i.e. uk.txt. I recommend using the "Get cookies.txt" extension from the Chrome web store.

```
amazondl.exe <mode> <asin> <region> <resolution> <bitrate> <codec>
amazondl.exe download B08GL1BVRT uk 1080 VBR H264
```

Regions: us, uk, jp, de, pv, pv-eu, pv-fe

Bitrates: CBR, VBR

Codecs: H264, H265

Modes: download, license

Download mode downloads, decrypts, and muxes the content, License mode gets and prints out the content keys

## Example
![gif](https://i.imgur.com/nHqTguc.gif)
