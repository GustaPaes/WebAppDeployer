# WebAppDeployer

An IIS management utility that automates the creation of application pools, sites, virtual paths, and application configurations during server releases.

### Usage:

#### 1) Create POOL

~~~ps1
WebAppDeployer.exe "appPoolName"
~~~

`"appPoolName"` = Pool name for creation

> Example: `"Bank"`

<br>

#### 2) Create Full

~~~ps1
WebAppDeployer.exe "appPoolName" "siteName" "virtualPath" "physicalPath" "concatString?"
~~~

`"appPoolName"` = Pool name for creation

> Example: `"Bank"`

`"siteName"` = IIS site name for publishing

> Example: `"Default Web Site"`

`"virtualPath"` = Virtual path

> Example: `"/ms/Bank"`

`"physicalPath"` = Physical path

> Example: `"C:\project\Web\microsservices\Bank"`

`"concatString?"` = String for pool concatenation (optional)