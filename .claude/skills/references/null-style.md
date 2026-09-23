# nullable のガイドライン

nullable : true の場合のガイドラインです。

## null forgiving operator ! の使用について

- null forgiving operator '!' は、コンパイラに対して「この値は null ではない」と伝えるために使用されますが、乱用は避けるべきです。
- ただし、Java/Kotlin/objective-c 由来のネイティブバインディングライブラリの使用時など、外部ライブラリが null 安全性を保障しない場合には、'!' の使用が許容されます。
- テストコードの場合は、'!' の使用が許容されます。