# 图鉴介绍

这是一个可以自动下载道具介绍的以撒内置图鉴。

把鼠标移动到地面上的道具，就可以看到效果。

图鉴信息来源于[灰机wiki](https://isaac.huijiwiki.com/wiki/%E9%81%93%E5%85%B7)。

英文图鉴信息来源于Fandom wiki:[Binding of Isaac: Rebirth Wiki is a Fandom Gaming Community](https://bindingofisaacrebirth.fandom.com/wiki/Binding_of_Isaac:_Rebirth_Wiki)

![预览](install_preview.gif)

# 缓存如何工作？

由于考虑到开源缓存同步机器人可能对wiki服务器造成不良影响，缓存同步机器人没有开源。机器人请求的`MediaWiki API`接口为`expandtemplates`，使用此接口对道具模板进行展开，请求结果直接保存为json文件作为缓存。您可以在代码中预置的URL里看到缓存内容。

如果想要使用wiki的api自行编写机器人，请务必遵守相应的礼仪规范。

# 版权声明

MIT LICENSE