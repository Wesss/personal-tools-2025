
<script lang="ts">
  import { onMount } from "svelte";

  let dataPromise : Promise<any>;

  onMount(async () => dataPromise = fetchData());

  async function fetchData() {
    const response = await fetch('http://localhost:5070/API'); // Replace with your API endpoint
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return await response.json();
  }
</script>

{#await dataPromise}
  Loading...
{:then data}
  {JSON.stringify(data, null, 2)}
{:catch error}
  {error}
{/await}
