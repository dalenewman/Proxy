Proxy
=====

Apache 2 license.

This is an Orchard CMS proxy module for securing private resources.  It's been tested on 1.7.1 and 1.8.  Currently it only supports GET and POST http methods.

I use it proxy internal/private Elasticsearch and Solr services.  It applies basic Orchard CMS security (via authentication), and also works with the item content permissions so you can grant view rights to specific roles.
