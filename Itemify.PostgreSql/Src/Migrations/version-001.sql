CREATE DATABASE itemic
    WITH 
    OWNER = postgres
    ENCODING = 'UTF8'
    TABLESPACE = pg_default
    CONNECTION LIMIT = -1;


CREATE TABLE public.items
(
    id uuid NOT NULL,
    "parentId" uuid NOT NULL,
    keywords text COLLATE pg_catalog."default",
    types text COLLATE pg_catalog."default",
    created timestamp without time zone NOT NULL,
    modified timestamp without time zone NOT NULL,
    CONSTRAINT "PK" PRIMARY KEY (id)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public.items
    OWNER to postgres;

-- Index: ID

-- DROP INDEX public."ID";

CREATE UNIQUE INDEX "items_parent_idx"
    ON public.items USING btree
    (id, id, id, id, id, id)
    TABLESPACE pg_default;