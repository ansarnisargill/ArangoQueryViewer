// my-server.js
import { handler } from './build/handler.js';
import express from 'express';
import { Database, aql } from "arangojs";

const PORT = process.env.PORT || 3000;
const app = express();
const db = new Database();

app.use(express.urlencoded({ extended: false }));
app.use(express.json());

app.post("/server/query", async (req, res) => {
    try {

        const formattedQuery = parseMultilineQuery(req.body.query);
        console.log({ orig: req.body.query, formattedQuery });

        const data = await db.query(aql`${formattedQuery}`);

        console.log({ data });
        res.json({ result: data });
    }
    catch (error) {
        console.error(error);
        res.status(500).json({ error: "internal server error, error:" + error });
    }
});
app.use(handler);

app.listen(PORT, () => {
    console.log(`listening on port http://localhost:${PORT}`);
});

function parseMultilineQuery(queryString) {
    return queryString;
    // Split the query string into a list of lines.
    const lines = queryString.split("\n");
  
    // Create a new array to store the parsed lines.
    const parsedLines = [];
  
    // Iterate over the lines.
    for (const line of lines) {
      // Check if the line ends with a quotation mark.
      if (line.endsWith("\"")) {
        // Remove the leading and trailing quotation marks.
        parsedLines.push(line.slice(1, -1));
      } else {
        // The line does not end with a quotation mark, so just add it to the parsed lines array.
        parsedLines.push(line);
      }
    }
  
    // Join the parsed lines together to form the original query string.
    return parsedLines.join("\n");
  }