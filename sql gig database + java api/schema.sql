DROP TABLE Act CASCADE;
CREATE TABLE Act (
  actID SERIAL PRIMARY KEY,
  actname VARCHAR(100),
  genre VARCHAR(20),
  members INTEGER,
  standardfee INTEGER
);

DROP TABLE Gig CASCADE;
CREATE TABLE Gig (
  gigID SERIAL PRIMARY KEY,
  venueid INTEGER,
  gigtitle VARCHAR(100),
  gigdate TIMESTAMP,
  gigstatus VARCHAR(10),
  FOREIGN KEY (venueid) REFERENCES Venue(venueid)
);

DROP TABLE Act_gig CASCADE;
CREATE TABLE Act_gig (
    actID INTEGER,
    gigID INTEGER,
    actfee INTEGER,
    ontime TIMESTAMP,
    duration INTEGER,
    PRIMARY KEY(actID,ontime),
    FOREIGN KEY (actID) REFERENCES Act(actID),
    FOREIGN KEY (gigID) REFERENCES Gig(gigID),
    CONSTRAINT checkTime CHECK (duration > 0 AND duration <= 120 AND (ontime::time + interval '1 minute' * duration) <= ('1-1-1 23:59:00') :: time)
);

DROP TABLE Venue CASCADE;
CREATE TABLE Venue (
  venueid SERIAL PRIMARY KEY,
  venuename VARCHAR(100),
  hirecost INTEGER,
  capacity INTEGER
);

DROP TABLE Gig_ticket CASCADE;
CREATE TABLE Gig_ticket (
  gigID INTEGER PRIMARY KEY,
  pricetype VARCHAR(2),
  cost INTEGER,
  FOREIGN KEY (gigID) REFERENCES Gig(gigID)
);

DROP TABLE Ticket CASCADE;
CREATE TABLE Ticket (
  ticketid SERIAL PRIMARY KEY,
  gigID INTEGER,
  pricetype VARCHAR(2),
  cost INTEGER NOT NULL DEFAULT 0,
  CustomerName VARCHAR(100),
  CustomerEmail VARCHAR(100),
  FOREIGN KEY (gigID) REFERENCES Gig_ticket(gigID)
);

--View of headline acts
CREATE OR REPLACE VIEW headliners AS
(SELECT ag.gigID, ag.actID, ontime FROM
  (Act_gig ag JOIN
    (SELECT gigId,MAX(ontime) latest FROM Act_gig GROUP BY gigID) sag
  ON ag.gigid = sag.gigid AND ag.ontime = sag.latest));

--Ensure tickets do not exceed venue capacity
CREATE OR REPLACE FUNCTION checkVenueOverflow() RETURNS trigger AS $$
    BEGIN
        IF (SELECT venue.capacity FROM gig NATURAL JOIN venue WHERE gig.gigID = NEW.gigID) < (SELECT COUNT(*) FROM ticket WHERE ticket.gigID = NEW.gigID) THEN
            RAISE EXCEPTION 'Sold more tickets then venue can hold';
        END IF;
        RETURN NEW;
    END;
$$ language plpgsql;

CREATE TRIGGER venueOverflow BEFORE INSERT ON ticket
    FOR EACH ROW
    EXECUTE PROCEDURE checkVenueOverflow();

--Ensure an act doesn't start before its gig
CREATE OR REPLACE FUNCTION checkValidOnTime() RETURNS trigger AS $$
    BEGIN
        IF NEW.ontime < (SELECT gigdate from gig WHERE gig.gigID=NEW.gigID) THEN
            RAISE EXCEPTION 'An act cannot start before its gig starts';
        END IF;
        RETURN NEW;
    END;
$$ language plpgsql;

CREATE TRIGGER validOnTime BEFORE INSERT OR UPDATE ON act_gig
    FOR EACH ROW
    EXECUTE PROCEDURE checkValidOnTime();

--Ensure two acts can't overlap
CREATE OR REPLACE FUNCTION preventOverlap() RETURNS trigger as $$
    DECLARE
        prev TIMESTAMP;
        next TIMESTAMP;
    BEGIN
        SELECT (ontime::time + duration * interval '1 minute') INTO prev FROM act_gig WHERE NEW.gigID = act_gig.gigID AND ontime < NEW.ontime ORDER BY ontime DESC LIMIT 1;
        SELECT (ontime::time) INTO next FROM act_gig WHERE NEW.gigID = act_gig.gigID AND NEW.ontime < ontime ORDER BY ontime LIMIT 1;
        IF (prev != NULL) AND (prev > NEW.ontime) OR (next != NULL) AND (NEW.ontime::time + NEW.duration * interval '1 minute' > next) THEN
            RAISE EXCEPTION 'Cannot have multiple acts playing at the same time';
        END IF;
        RETURN NEW;
    END;

    $$ language plpgsql;

    CREATE TRIGGER overlap BEFORE INSERT ON act_gig
        FOR EACH ROW
        EXECUTE PROCEDURE preventOverlap();
