## これはなに
- 区割りシステムです。以下の機能があります（2023年末 記載。適当です。そのうちいい感じに書きます）
	- 領域データが記入されているxmlファイルを読み込みます
	- 領域をある太さの長方形で埋め尽くします。
	- 埋め尽くす長方形の幅や方向などはいい感じに自分で設定できます
	- 作業エリアをいい感じに整形してアクティビティとして出力できます

## 振動ローラーのパラメータファイル
- json形式
- exeファイルと同じ階層のsettings/MachineInfo.json

| 変数名 | 説明 |
|--------|------|
| Name | 重機名称 |
| Description | 任意のコメント |
| WheelType | 片鉄輪(0)／両鉄輪(1) |
| LaneWidth | 鉄輪幅 (m) |
| Wheelbase | ホイールベース (m) |
| FrontOverhang | フロントオーバーハング (m) |
| RearOverhang | リアオーバーハング (m) |
| FrontRearMargin | 前後マージン (m) |
| LeftRightMargin | 左右マージン (m) |
| LaneChangeLength | 作業可能長さ (m) |
| LapWidth | ラップ幅 (m) |
| FrontEdgeOffset | 前方端部余裕代 (m) |
| BackEdgeOffset | 後方端部余裕代 (m) |
| WorkAreaLapLength | ラップ調整値 (m) |
| InitialMoveMarginFront | 初期配置用拡幅 作業進捗側 (m) |
| InitialMoveMarginBack | 初期配置用拡幅 作業進捗反対側 (m) |
| RefSpeed | 参照速度 (km/h) |
| EdgeSpeed | 端部速度 (km/h) |
| RepeatNum | 転圧繰返し回数 |

