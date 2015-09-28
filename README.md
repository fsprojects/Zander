# Zander [![Build status](https://ci.appveyor.com/api/projects/status/u3nlfqfahv8w0tjw/branch/master?svg=true)](https://ci.appveyor.com/project/wallymathieu/zander/branch/master) [![Build Status](https://travis-ci.org/wallymathieu/Zander.svg?branch=master)](https://travis-ci.org/wallymathieu/Zander)

Named after the fish: Zander. It's a small library to ease with parsing structured blocks of information 
within a 2-dimensional matrix of information. Typically you get this sort of information from report generators.
You might still want to extract this information programmatically, thus the need for the fish.

## What problem does this library solve?

When you have data in a structured format, but with different blocks of information. A very simple example is the following:

<table >
<tbody><tr class="report-row"><th></th><th>&nbsp;</th><th>&nbsp;</th><th>&nbsp;</th><th>&nbsp;</th><th>&nbsp;</th><th>Report Title</th><th>&nbsp;</th><th>&nbsp;</th><th>&nbsp;</th><th>16/09/15 16:17</th><th>Page: 1</th></tr>
<tr><td>Company AB</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>Some text</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>that goes on and explains the report</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>Id</td><td>&nbsp;</td><td>Value</td><td>Type</td><td>&nbsp;</td><td>&nbsp;</td><td>Attribute 1</td><td>&nbsp;</td><td>Attribute 2</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1244</td><td>&nbsp;</td><td>25</td><td>A</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1244</td><td>&nbsp;</td><td>25</td><td>B</td><td>&nbsp;</td><td>&nbsp;</td><td>255</td><td>&nbsp;</td><td>155</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1244</td><td>&nbsp;</td><td>25</td><td>C</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1250</td><td>&nbsp;</td><td>25</td><td>B</td><td>&nbsp;</td><td>&nbsp;</td><td>255</td><td>&nbsp;</td><td>100</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1250</td><td>&nbsp;</td><td>25</td><td>C</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr class="report-row"><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>Report Title</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>16/09/15 16:17</td><td>Page: 2</td></tr>
<tr><td>Company AB</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>Some text</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>that goes on and explains the report</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>Id</td><td>&nbsp;</td><td>Value</td><td>Type</td><td>&nbsp;</td><td>&nbsp;</td><td>Attribute 1</td><td>&nbsp;</td><td>Attribute 2</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1251</td><td>&nbsp;</td><td>25</td><td>A</td><td>&nbsp;</td><td>&nbsp;</td><td>255</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1251</td><td>&nbsp;</td><td>25</td><td>B</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>130</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1251</td><td>&nbsp;</td><td>25</td><td>C</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1260</td><td>&nbsp;</td><td>25</td><td>A</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1260</td><td>&nbsp;</td><td>25</td><td>B</td><td>&nbsp;</td><td>&nbsp;</td><td>255</td><td>&nbsp;</td><td>15</td><td>&nbsp;</td><td>&nbsp;</td></tr>
<tr><td>&nbsp;</td><td>1260</td><td>&nbsp;</td><td>25</td><td>C</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td>&nbsp;</td><td></td></tr>
</tbody></table>

But the structure of the block layout might change from "page" to "page".

## How do you match?

### Match columns

* Use ```_``` to indicate that there should be an empty column
* Use ```"Some constant"``` or ```constant``` to indicate a column with a constant value
* Use ```@Value``` to indicate that you want the value on that column

### Match rows
In order to match rows you supply the row specification with a name by postfixing with ``` : title```
If you want the row to match many rows with the same format you add a '+' : ``` : title+```

### How does it look?

How do you use this library to extract the information above? You use the parser builder:
```c#
using Zander;
...
 var parsed = new BlockEx( @" _          _ _ _ _ _ ""Report Title"" _  _  _  @Time @Page : report_title
                                ""Company AB"" _ _ _ _ _ _                _ _ _ _  _          : company
                                    @Text      _ _ _ _ _ _                _ _ _ _  _          : text+
                                  _         Id _  Value  Type _ _ ""Attribute 1"" _ ""Attribute 2"" _  _ : header
                                  _        @Id _ @Value @Type _ _ @Attribute1     _ @Attribute2     _  _ : row+
                    ")
                .Matches(arrayOfArrays);
```

This will give you structured information that will be easy to consume.

