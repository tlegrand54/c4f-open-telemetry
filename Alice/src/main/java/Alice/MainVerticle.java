package Alice;

import io.vertx.core.AbstractVerticle;
import io.vertx.core.Promise;

public class MainVerticle extends AbstractVerticle {

  @Override
  public void start(Promise<Void> startPromise) {
    vertx.createHttpServer().requestHandler(req -> req.response()
      .putHeader("content-type", "text/plain")
      .end("Rogers!")).listen(2000, http -> {
      if (http.succeeded()) {
        startPromise.complete();
        System.out.println("Alice's ready to receive her mails...");
      } else {
        startPromise.fail(http.cause());
      }
    });
  }
}
