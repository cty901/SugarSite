# SugarSite 一个支持移动端的企业站，并且后期会集成BBS
 **
技术架构 指挥官模式** 
![输入图片说明](http://images2015.cnblogs.com/blog/746906/201611/746906-20161127171647159-1573188157.jpg "在这里输入图片标题")

 **称词解释：** 

ViewAction ：类型为 ActionResult类型的Action

ApiAction： 类型为JsonResult类型的Action,在这里作为一个业务接口使用

Pack： 为一个业务块，并且业务块和业务块之间是隔离的，每个业务块有多个 ApiAction 和多个ViewAction，

业务块与业务块之间的通信用的是RestSharp调用其它Pack的ApiAction

Outsourcing：业务块中的一些公用代码，可以是一个类也可以是一个文件夹，根据项目复杂度而定


 