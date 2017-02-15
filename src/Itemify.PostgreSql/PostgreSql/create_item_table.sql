-- Table: public."{{ tableName }}"

-- DROP TABLE public."{{ tableName }}";

CREATE TABLE public."{{ tableName }}"
(
    id uuid NOT NULL,
    "parentId" uuid NOT NULL,
    type int,
    typeName varchar(50),
    keywords text COLLATE pg_catalog."default",
    types text COLLATE pg_catalog."default",
    created timestamp without time zone NOT NULL,
    modified timestamp without time zone NOT NULL,
    CONSTRAINT "PK" PRIMARY KEY (id, type)
)
WITH (
    OIDS = FALSE
)
TABLESPACE pg_default;

ALTER TABLE public."{{ tableName }}"
    OWNER to postgres;

-- Index: ID

-- DROP INDEX public."ID";

CREATE UNIQUE INDEX "ID"
    ON public."{{ tableName }}" USING btree
    (id, type)
    TABLESPACE pg_default;