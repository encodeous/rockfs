<svelte:options tag="rfs-inspector" />
<script>
    import {get} from 'svelte/store';
    import { activeItems } from './stores.js'
    window.activeItems = activeItems;
</script>

<main>
    {#if get(activeItems) == null || get(activeItems).length === 0}
        Select one or more items to view details.
    {/if}
</main>