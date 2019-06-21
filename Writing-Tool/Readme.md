# REVIVE USB MICRO

## HIDブートローダ書き込みソフト（HIDBootLoader.exe）の使い方

### 1. このソフトで何が出来るのか

REVIVE USB MICROはMicrochip社のPICと言うマイコンを用いて作られています。  
通常、PICマイコンにソフトを書き込む為には「ライタ」と呼ばれる書き込み装置が必要です。  
（Microchip社が出しているPICkitや、秋月電子のキットであるAKI-PICプログラマーなどが有名です）  

このソフトを用いれば、特別なライタを必要とせず、USBから直接マイコンのソフトを書き換える事ができます。  
ただし、その為にはマイコンにBootLoaderと呼ばれる特別なソフトがあらかじめ書き込まれている必要があり、またUSB経由で書き込むソフトもBootLoaderに対応したものになっていなければなりません。  
あらかじめ書き込まれるBootLoader自体は、「ライタ」で書き込まなければなりません。  

REVIVE USB MICROに付属しているマイコンにはこのBootLoaderと言う特別なソフトがあらかじめ書かれています。よって、特別なライタをお持ちでなくてもUSB経由でソフトを書き込むことができます。  
また、Assembly Deskで公開されているREVIVE USB MICRO用のソフトはこのBootLoaderでの書き込みに対応したソフトになっています。  

このBootLoaderを使うことで、USB経由で簡単にソフトを書き換えて、REVIVE USBのソフトを様々に書き換える事ができます。  

### 2. 準備

Assembly DeskのサイトからHIDBootLoader.exeをダウンロードします。  
（これは、Microchipさんが公開しているライブラリ「MCHPFSUSB」の中にある物と同一です）  

このソフトは.NET framework 2.0を用いて作成されています。  
お手持ちのパソコンに.NET frameworkがインストールされていない場合、MicrosoftさんのHPからこれらをダウンロードし、インストールしておく必要があります。  
“Microsoft .NET framework”等のキーワードで検索すると出てきます。  

書き込みたいソフトもダウンロードしておきます。  

### 3. ソフトの書き込み

最初に、設定ツールからREVIVE USB MICROをBOOTモードにします。
![](http://bit-trade-one.co.jp/wp/wp-content/uploads/2019/06/01soft.jpg)
右下の「Update」ボタンをクリックしてください。
![](http://bit-trade-one.co.jp/wp/wp-content/uploads/2019/06/02update.jpg)
するとこのようにダイアログが表示されます。
OKをクリックすると、REVIVE USB MICROがBOOTモードになります。

HIDBootLoader.exeを立ち上げます。  

![](http://bit-trade-one.co.jp/wp/wp-content/uploads/2019/06/03HIDTool.png)  
デバイスが認識されると上図の様に「Device attached」と表示されます。  
初めてBOOTモードで接続した時には、自動的にPCにドライバがインストールされます。（約１分程時間かかります）  

「Open Hex File」を押します。  
書き込みたいHexファイルを選択します。  
![](http://bit-trade-one.co.jp/wp/wp-content/uploads/2019/06/04choose.png)  
「Program/Verify」を押します。  
ソフトが書き込まれます。  
![](http://bit-trade-one.co.jp/wp/wp-content/uploads/2019/06/05Program.png)  
USBケーブルを抜き差しすると、書き込んだソフトが起動します。  

