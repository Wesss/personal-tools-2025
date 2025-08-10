<script lang="ts">
  let status = "";
  let success = true;

  // TODO WESD make an actual ajax postback to import decks
  async function importDecks() {
    status = "click";
  }
</script>

<h1>Import Decks</h1>
<textarea name="import" rows="30" cols="100"></textarea>
<button onclick={importDecks}>Submit</button>
{#if status}
  <!-- TODO WESD color based on status -->
  <div class="{success ? 'success' : 'error'}">
    {status}
  </div>
{/if}