# REVIVE USB

## HIDブートローダ書き込みソフト（HIDBootLoader.exe）の使い方

### 1. このソフトで何が出来るのか

REVIVE USBはMicrochip社のPICと言うマイコンを用いて作られています。  
通常、PICマイコンにソフトを書き込む為には「ライタ」と呼ばれる書き込み装置が必要です。  
（Microchip社が出しているPICkitや、秋月電子のキットであるAKI-PICプログラマーなどが有名です）  

このソフトを用いれば、特別なライタを必要とせず、USBから直接マイコンのソフトを書き換える事ができます。  
ただし、その為にはマイコンにBootLoaderと呼ばれる特別なソフトがあらかじめ書き込まれている必要があり、またUSB経由で書き込むソフトもBootLoaderに対応したものになっていなければなりません。  
あらかじめ書き込まれるBootLoader自体は、「ライタ」で書き込まなければなりません。  

REVIVE USBに付属しているマイコンにはこのBootLoaderと言う特別なソフトがあらかじめ書かれています。よって、特別なライタをお持ちでなくてもUSB経由でソフトを書き込むことができます。  
また、Assembly Deskで公開されているREVIVE USB用のソフトはこのBootLoaderでの書き込みに対応したソフトになっています。  

このBootLoaderを使うことで、USB経由で簡単にソフトを書き換えて、REVIVE USBのソフトを様々に書き換える事ができます。  

### 2. 準備

Assembly DeskのサイトからHIDBootLoader.exeをダウンロードします。  
（これは、Microchipさんが公開しているライブラリ「MCHPFSUSB」の中にある物と同一です）  

このソフトは.NET framework 2.0を用いて作成されています。  
お手持ちのパソコンに.NET frameworkがインストールされていない場合、MicrosoftさんのHPからこれらをダウンロードし、インストールしておく必要があります。  
“Microsoft .NET framework”等のキーワードで検索すると出てきます。  

書き込みたいソフトもダウンロードしておきます。  

### 3. ソフトの書き込み

HIDBootLoader.exeを立ち上げます。  
![](https://lh5.googleusercontent.com/-LoOXYx-U6JU1DHq827MB8_Rr_vVrsVM1tQ-15Qt62flYZ1t7dUdVXaBLf_9_d8waA406JswOexrPiNcTl_VAKZCXRSiR_85DPMlKXC-l3Gu-h_cQFDRFD0I)  
次に、REVIVE USBのショートピンをBOOTに設定し、USBケーブルを接続します。  
（REVIVE USBが既に差し込んであった場合には一度抜いてからショートピンをBOOTに変更し、再度USBケーブルを差し込みます）  
![](https://lh4.googleusercontent.com/pMICZ6pBKk9dScJrVAqOMX_rBjZCpKkjMuCiYZ_BEP-B4R-zhiyMBPBcUIJZXGn5NGJHRjlyVFimkgDp8CANROQepg-DbWDXUkuaa6Q-D2oAZhN8uEB5xmSK)  
ショートピンをBOOTの方にし、USB ケーブルを接続します。  
![](https://lh5.googleusercontent.com/WIZEQkqjACZQy8cXOTtHcCphWbtTRoAqtJRJBU4nBQPqal6o29Db1RgKaR72U6k9Pn_UAZ2AqRlzY-mzc5lbXGeQh_NJplQKMsG8f_8lXTqhUVoLEP2wsSWA)  
デバイスが認識されると上図の様に「Device attached」と表示されます。  
この表示が出たら「上」ボタンは離しても大丈夫です。  
始めてBOOTモードで接続した時には、自動的にPCにドライバがインストールされます。（約１分程時間かかります）  

「Open Hex File」を押します。  
書き込みたいHexファイルを選択します。  
![](https://lh3.googleusercontent.com/M8iLRDr4ywckCrp2ERl6yFlg6HMsUwJquLnQfvSFLhfWz28tyif9Cr_iAMP5f9j9JymHWjI8bpvXKoDS78xEfp4jwaLLE6WDRqgqgX6bYB_RdXZl32eBdSoo)  
「Program/Verify」を押します。  
ソフトが書き込まれます。  
![](https://lh5.googleusercontent.com/QnN9dpeDCuivzr7NDv4quTuaJwTE5Ut8tlmfiqqxeoutUQGiw5qZyKojG7Qyg5cBqKgqW9vWMpxk2g-Z3HWYOG1ZpEzaXaJ6gjWSBrTt4eZXBmrXhTXVmlFj)  
ショートピンを通常状態に戻し、USBケーブルを抜き差しするか、「Reset Device」を押すと、書き込んだソフトが起動します。  

