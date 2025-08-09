<script lang="ts">
	import type { Component } from 'svelte';
  import Counter from './lib/Counter.svelte'
  import ApiTest from './lib/ApiTest.svelte'

  const menuDict: { [key: string]: Component } = {
    "Counter" : Counter,
    "ApiTest" : ApiTest
  };

	let activePage : Component = menuDict["Counter"];

  function setPage(event: Event, component: Component) {
    event.preventDefault();
    activePage = component;
  }
</script>

<main>
  <ul id="menu">
    {#each Object.keys(menuDict) as key}
      <li><a href="/" onclick={(event) => setPage(event, menuDict[key])}>{key}</a></li>
    {/each}
  </ul>

  {#if !activePage}
    <h1>
      Page Not Found
    </h1>
  {:else}
    <svelte:component this={activePage} />
  {/if}
</main>

<style>
  main {
    margin: 25px;
    min-width: 480px;
    min-height: 100vh;
  }
</style>