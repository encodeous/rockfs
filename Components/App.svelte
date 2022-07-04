<svelte:options tag="svelte-app" />
<script>
    export let name;
    export let id;
</script>

<main class="shad">
    <h1 id="{id}">Hello {name}!</h1>
    <h6>
        <slot/>
    </h6>
</main>

<style>
    h1 {
        font-size: 5em;
    }
</style>