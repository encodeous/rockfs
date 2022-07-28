import inspector from './inspector.svelte'
import explorer from './explorer.svelte'
import component from "svelte-tag"
new component({component:explorer,tagname:"rfs-explorer",shadow:false})
new component({component:inspector,tagname:"rfs-inspector",shadow:false})