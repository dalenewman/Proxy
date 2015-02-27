Proxy
=====

This is an Orchard CMS 1.7+ module for securing 
private (or just internal) resources.  It is 
released under the Apache 2 license.

---

Out of the box, this module handles GET and POST 
requests.  If you need to support other methods, 
you have to make some _web.config_ adjustments.

###Web.Config Adjustments

####WebDAV
The WebDAV module intefers with `PUT`, and `DELETE` requests.
So, if you want to allow `PUT` and `DELETE` through 
your proxy, you have to disable WebDav.  This is done 
by modifying Orchard's _web.config_.

<pre class="prettyprint" lang="xml">
&lt;modules&gt;
    ...
    &lt;remove name=&quot;WebDAVModule&quot;/&gt;
&lt;/modules&gt;
</pre>

<pre class="prettyprint" lang="xml">
&lt;handlers&gt;
    ...
    &lt;remove name=&quot;WebDAV&quot;/&gt;
&lt;/handlers&gt;
</pre>

####Invalid Path Characters

By default, ASP.NET considers some characters in 
a path to be invalid. These characters are `<`, `>`, `*`, `%`, `:`, and `&`.

If your proxied resource paths require any of these 
characters, you may need to edit `requestPathInvalidCharacters` 
in `httpRuntime`.

The example below 
has the default invalid characters, 
minus the `*` character.

<pre class="prettyprint" lang="xml">
&lt;system.web&gt;
    &lt;httpRuntime ... requestPathInvalidCharacters=&quot;&amp;lt;,&amp;gt;,%,:,&amp;amp;&quot; /&gt;
&lt;/system.web&gt;
</pre>
