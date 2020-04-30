# UI.SyntaxBox
Attach syntax highlighting to a WPF TextBox.

The SyntaxBox behavior attaches syntax highlighting capabilities to an existing .NET Framework WPF TextBox.
It supports two distinct rule types for syntax syntax recognition, RegexRule and a much faster, custom KeywordRule based on the Aho-Corasick algorithm, all configured directly in XAML.

Here an example of using the SyntaxBox for syntax highlighting ofsomething approximating C# code:

// xmlns:syntax="clr-namespace:UI.SyntaxBox;assembly=UI.SyntaxBox"
// ...
<TextBox 
    x:Name="textBox"
    syntax:SyntaxBox.Enable="True"
    syntax:SyntaxBox.ExpandTabs="True"
    syntax:SyntaxBox.AutoIndent="True"
    syntax:SyntaxBox.ShowLineNumbers="True"
    syntax:SyntaxBox.LineNumbersBackground="LightGray"
    syntax:SyntaxBox.LineNumbersForeground="SlateGray"            
    AcceptsReturn="True" 
    AcceptsTab="True"
    VerticalScrollBarVisibility="Auto"
    HorizontalScrollBarVisibility="Auto"
    Padding="0,0,0,0"
    FontFamily="Consolas"
    FontSize="11pt">
    <syntax:SyntaxBox.SyntaxDrivers>
        <syntax:SyntaxConfig>
            <!-- Keywords -->
            <syntax:KeywordRule Foreground="Blue"
                Keywords="abstract,as,base,bool,break,byte,case,catch,char,checked,class,const,continue,decimal,default,delegate,do,double,else,enum,event,explicit,extern,false,finally,fixed,float,for,foreach,goto,if,implicit,in,int,interface,internal,is,lock,long,namespace,new,null,object,operator,out,override,params,private,protected,public,readonly,ref,return,sbyte,sealed,short,sizeof,stackalloc,static,string,struct,switch,this,throw,true,try,typeof,uint,ulong,unchecked,unsafe,ushort,using,using,static,virtual,void,volatile,while,get,set,yield,var"
            />
                        
            <!-- Operators -->
            <syntax:KeywordRule Foreground="Purple"
                Keywords="!,+,-,/,*,%,=,&amp;,^,|,&lt;,&gt;"
                WholeWordsOnly="False"
            />
            <!--<syntax:RegexRule Op="Line" Foreground="Purple" Pattern="[\!+-\/\*%=&amp;\^|\?&lt;&gt;]" />-->
                        
            <!-- Preproc directives -->
            <syntax:RegexRule Op="Line" Foreground="Gray" Pattern="^\s*#.*" />

            <!-- String -->
            <syntax:RegexRule Op="Line" Foreground="Maroon" Pattern="&quot;(?:[^&quot;\\]|\\.)*&quot;" />

            <!-- Verbatim string -->
            <syntax:RegexRule Op="Block" Foreground="Maroon" Pattern="@&quot;(?:[^&quot;])*&quot;" />

            <!-- Comment -->
            <syntax:RegexRule Op="Line" Foreground="Green" Pattern="//.*" />

            <!-- Multiline comment -->
            <syntax:RegexRule Op="Block" Foreground="Green" Pattern="(?m)/\*[^*]*\*+(?:[^\/*][^*]*\*+)*/" />

        </syntax:SyntaxConfig>
    </syntax:SyntaxBox.SyntaxDrivers>
</TextBox>
