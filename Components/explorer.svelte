<script>
    import {get} from 'svelte/store';
    import { activeItems } from './stores.js'
    import {onMount} from "svelte";
    import {extensions} from "./extensions.js";

    window.activeItems = activeItems;
    let lastClickedItem = "";
    let lastClickedTime = 0;
    function getUrl(resource){
        let basePath = window.explorerData.CurPath;
        if(resource.IsDirectory){
            return basePath + resource.RelativePath + "/"
        }
        else{
            return basePath + resource.RelativePath;
        }
    }
    function findFile(resPath){
        return get(activeItems).filter((val) =>{
            return val.RelativePath === resPath;
        })[0];
    }
    function fileClicked(resource, e){
        let curTime = new Date().getTime();
        let curItem = resource.RelativePath;
        if(lastClickedItem === curItem){
            if(curTime - lastClickedTime <= 450){
                window.location = getUrl(resource);
            }else{
                if(get(activeItems).includes(resource.RelativePath)){
                    activeItems.set([]);
                }else{
                    activeItems.set([resource.RelativePath]);
                }
            }
        }
        else{
            let isShift = e.shiftKey;
            let isControl = e.ctrlKey;
            if(isControl){
                let items = new Set(get(activeItems));
                if(items.has(resource.RelativePath)){
                    items.delete(resource.RelativePath);
                }else{
                    items.add(resource.RelativePath);
                }
                activeItems.set([...items]);
            }else if(isShift){
                let items = get(activeItems);
                let minIndex = 10000;
                let maxIndex = -1;
                for(let i of items){
                    minIndex = Math.min(minIndex, window.explorerData.Resources.findIndex((e) => e.RelativePath === i));
                    maxIndex = Math.max(maxIndex, window.explorerData.Resources.findIndex((e) => e.RelativePath === i));
                }
                minIndex = Math.min(minIndex, window.explorerData.Resources.findIndex((e) => e.RelativePath === curItem));
                maxIndex = Math.max(maxIndex, window.explorerData.Resources.findIndex((e) => e.RelativePath === curItem))
                let built = [];
                for(let i = minIndex; i <= maxIndex; i++){
                    built.push(window.explorerData.Resources[i].RelativePath);
                }
                activeItems.set(built);
            }else{
                if(get(activeItems).includes(resource.RelativePath)){
                    activeItems.set([]);
                }else{
                    activeItems.set([resource.RelativePath]);
                }
            }
        }
        lastClickedTime = curTime;
        lastClickedItem = curItem;
    }
</script>

<div id="file-explorer" class="bg-dark files prevent-select">
    <table class="table table-dark table-hover">
        <thead class="table-light">
        <tr>
            <th scope="col"></th>
            <th scope="col">Name</th>
            <th scope="col">Size</th>
            <th scope="col">Last Updated</th>
        </tr>
        </thead>
        <tbody>
        {#each window.explorerData.Resources as resource}            
            <tr class="rfs-file-row {($activeItems).includes(resource.RelativePath) ? 'rfs-file-row-selected' : ''}" key="{resource.Name}" on:click={(e) => fileClicked(resource, e)}>
                <td style="width: 20px">
                    {#if resource.IsDirectory}
                        <span class="fiv-viv fiv-icon-folder"></span>
                    {:else}
                        {#if extensions.includes(resource.Info.Extension)}
                            <span class="fiv-viv fiv-icon-{resource.Info.Extension}"></span>
                        {:else}
                            <span class="fiv-viv fiv-icon-blank"></span>
                        {/if}
                    {/if}
                </td>
                <td>
                    {resource.Name}
                </td>
                <td>
                    {#if !resource.IsDirectory}
                        {resource.Info.DisplaySize}
                    {/if}
                </td>
                <td>
                    {resource.Info.DisplayUpdate}
                </td>
            </tr>
        {/each}
        </tbody>
    </table>
</div>

<style>
    .rfs-file-row{
        cursor: pointer;
        max-height: 41px;
    }
    td{
        background-color: transparent;
    }
    .rfs-file-row-selected{
        background-color: #1b6ec2 !important;
    }
    .rfs-file-row-selected:hover{
        --bs-table-accent-bg: #1e5486 !important;
        background-color: #1e5486 !important;
    }
    .prevent-select {
        -webkit-user-select: none; /* Safari */
        -ms-user-select: none; /* IE 10 and IE 11 */
        user-select: none; /* Standard syntax */
    }
</style>