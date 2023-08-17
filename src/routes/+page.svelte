<script lang="ts">
	let collectionName = '';
	let query = '';

	async function run() {
		if (!collectionName || !query) {
			alert('connection name or query are empty');
			return;
		}
		let result = await sendLocalRequest(
			'POST',
			'/server/query',
			JSON.stringify({ collection: collectionName, query })
		);
	}
	async function sendLocalRequest(method: string, url: string, body: BodyInit | null = null) {
		try {
			url = `http://localhost:3000${url}`;
			let request: RequestInit = {
				method: method,
				headers: {
					'Content-Type': 'application/json'
				}
			};
			if (body) {
				request.body = body;
			}
			let response = await fetch(url, request);
			let json = await response.json();
			console.log({ json });
			return json;
		} catch (error) {
			console.error(error);
			return null;
		}
	}
</script>

<form>
	<label for="about">Graph</label>
	<input placeholder="collection name" bind:value={collectionName} />

	<label for="about">Enter Query</label>
	<textarea placeholder="AQL Query" spellcheck="false" data-ms-editor="true" bind:value={query} />
</form>
<button on:click={run}>Run</button>
