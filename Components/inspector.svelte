<script>
    import {get} from 'svelte/store';
    import { activeItems } from './stores.js'
    window.activeItems = activeItems;
    import {extensions} from "./extensions.js";
    
    function grab(relPath){
        return window.explorerData.Resources.filter((x) => {
            return x.RelativePath == relPath;
        })[0];
    }

    function trunc(str, num) {
        if (str.length > num) {
            return str.slice(0, num) + "...";
        } else {
            return str;
        }
    }
    
    $: single = $activeItems.length == 1 ? grab($activeItems[0]) : {};
</script>

<main>
    {#if $activeItems == null || $activeItems.length === 0}
        Select one or more items to view details.
    {:else}
        {#if $activeItems.length === 1}
            <h5 class="font-monospace">
                {trunc(single.Name, 25)}
            </h5>
            <div class="d-flex w-100 justify-content-center mt-5">
                {#if single.IsDirectory}
                    <span class="fiv-viv fiv-icon-folder"></span>
                {:else}
                    {#if extensions.includes(single.Info.Extension)}
                        <span class="fiv-viv fiv-icon-{single.Info.Extension}"></span>
                    {:else}
                        <span class="fiv-viv fiv-icon-blank"></span>
                    {/if}
                {/if}
            </div>
        {:else}
            
        {/if}
    {/if}
</main>

<style>
    .fiv-viv{
        height: 180px;
        width: 180px;
    }
</style>